﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CoreSharp.Breeze.Query {
  public class EntityQuery {

    private string _resourceName;
    private BasePredicate _wherePredicate;
    private OrderByClause _orderByClause;
    private ExpandClause _expandClause;
    private SelectClause _selectClause;
    private int? _skipCount;
    private int? _takeCount;
    private bool? _inlineCountEnabled;
    private Dictionary<string, object> _parameters;
    private Type _entityType;

    public EntityQuery() {

    }


    /**
     * Materializes the serialized json representation of an EntityQuery.
     * @param json The serialized json version of the EntityQuery.
     */
    public EntityQuery(string json) {
      if (json == null || json.Length == 0) {
        return;
      }
      Dictionary<string, object> qmap;
      try {
        var dmap = JsonHelper.Deserialize(json);
        qmap= (Dictionary<string, object>)dmap;
      } catch (Exception) {
        throw new Exception(
                "This EntityQuery ctor requires a valid json string. The following is not json: "
                        + json);
      }

      this._resourceName = GetMapValue<string>(qmap, "resourceName");
      this._skipCount = GetMapInt(qmap, "skip");
      this._takeCount = GetMapInt(qmap, "take");
      this._wherePredicate = BasePredicate.PredicateFromMap( GetMapValue<Dictionary<string, object>>(qmap, "where"));
      this._orderByClause = OrderByClause.From(GetMapValue<List<object>>(qmap,"orderBy"));
      this._selectClause = SelectClause.From(GetMapValue<List<object>>(qmap, "select"));
      this._expandClause = ExpandClause.From(GetMapValue<List<object>>(qmap, "expand"));
      this._parameters = GetMapValue<Dictionary<string, object>>(qmap, "parameters");
      this._inlineCountEnabled = GetMapValue<bool?>(qmap, "inlineCount");

    }


    /**
     * Copy constructor
     * @param query
     */
    public EntityQuery(EntityQuery query) {
      this._resourceName = query._resourceName;
      this._skipCount = query._skipCount;
      this._takeCount = query._takeCount;
      this._wherePredicate = query._wherePredicate;
      this._orderByClause = query._orderByClause;
      this._selectClause = query._selectClause;
      this._expandClause = query._expandClause;
      this._inlineCountEnabled = query._inlineCountEnabled;
      this._parameters = query._parameters;

    }



    /**
     * Return a new query based on this query with an additional where clause added.
     * @param json Json representation of the where clause.
     * @return A new EntityQuery.
     */
    public EntityQuery Where(string json) {
      var qmap = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
      var pred = BasePredicate.PredicateFromMap(qmap);
      return this.Where(pred);
    }

    private T GetMapValue<T>(IDictionary<string, object> map, string key) {
      if (map.ContainsKey(key)) {
        return (T)map[key];
      } else {
        return default(T);
      }
    }

    private int? GetMapInt(IDictionary<string, object> map, string key) {
      if (map.ContainsKey(key)) {
        return Convert.ToInt32(map[key]);
      } else {
        return null;
      }
    }

    /**
     * Return a new query based on this query with an additional where clause added.
     * @param predicate A Predicate representing the where clause to add.
     * @return A new EntityQuery.
     */
    public EntityQuery Where(BasePredicate predicate) {
      var eq = new EntityQuery(this);
      if (eq._wherePredicate == null) {
        eq._wherePredicate = predicate;
      } else if (eq._wherePredicate.Operator == Operator.And) {
        var andOrPred = (AndOrPredicate)eq._wherePredicate;
        var preds = new List<BasePredicate>(andOrPred.Predicates);
        preds.Add(predicate);
        eq._wherePredicate = new AndOrPredicate(Operator.And, preds);
      } else {
        eq._wherePredicate = new AndOrPredicate(Operator.And,
                eq._wherePredicate, predicate);
      }
      return eq;
    }

    /**
     * Return a new query based on this query with the specified orderBy clauses added.
     * @param propertyPaths A varargs array of orderBy clauses ( each consisting of a property path and an optional sort direction).
     * @return A new EntityQuery.
     */
    public EntityQuery OrderBy(params string[] propertyPaths) {
      return OrderBy(propertyPaths.ToList());
    }

    /**
     * Return a new query based on this query with the specified orderBy clauses added.
     * @param propertyPaths An List of orderBy clauses ( each consisting of a property path and an optional sort direction).
     * @return A new EntityQuery.
     */
    public EntityQuery OrderBy(List<string> propertyPaths) {
      var eq = new EntityQuery(this);
      if (this._orderByClause == null) {
        eq._orderByClause = new OrderByClause(propertyPaths);
      } else {
        var propPaths = this._orderByClause.PropertyPaths.ToList();
        propPaths.AddRange(propertyPaths);
        eq._orderByClause = new OrderByClause(propPaths);
      }
      return eq;
    }

    /**
     * Return a new query based on this query with the specified expand clauses added.
     * @param propertyPaths A varargs array of expand clauses ( each a dot delimited property path).
     * @return A new EntityQuery.
     */
    public EntityQuery Expand(params string[] propertyPaths) {
      return Expand(propertyPaths.ToList());
    }

    /**
     * Return a new query based on this query with the specified expand clauses added.
     * @param propertyPaths A list of expand clauses (each a dot delimited property path).
     * @return A new EntityQuery.
     */
    public EntityQuery Expand(List<string> propertyPaths) {
      var eq = new EntityQuery(this);
      if (this._expandClause == null) {
        eq._expandClause = new ExpandClause(propertyPaths);
      } else {
        // think about checking if any prop paths are duped.
        var propPaths = this._expandClause.PropertyPaths.ToList();
        propPaths.AddRange(propertyPaths);
        eq._expandClause = new ExpandClause(propPaths);
      }
      return eq;
    }

    /**
     * Return a new query based on this query with the specified select (projection) clauses added.
     * @param propertyPaths A varargs array of select clauses (each a dot delimited property path).
     * @return A new EntityQuery.
     */
    public EntityQuery Select(params string[] propertyPaths) {
      return Select(propertyPaths.ToList());
    }

    /**
     * Return a new query based on this query with the specified select (projection) clauses added.
     * @param propertyPaths A list of select clauses (each a dot delimited property path).
     * @return A new EntityQuery.
     */
    public EntityQuery Select(IEnumerable<string> propertyPaths) {
      var eq = new EntityQuery(this);
      if (this._selectClause == null) {
        eq._selectClause = new SelectClause(propertyPaths);
      } else {
        // think about checking if any prop paths are duped.
        var propPaths = this._selectClause.PropertyPaths.ToList();
        propPaths.AddRange(propertyPaths);
        eq._selectClause = new SelectClause(propPaths);
      }
      return eq;
    }


    /**
     * Return a new query based on this query that limits the results to the first n records.
     * @param takeCount The number of records to take.
     * @return A new EntityQuery
     */
    public EntityQuery Take(int takeCount) {
      var eq = new EntityQuery(this);
      eq._takeCount = takeCount;
      return eq;
    }

    /**
     * Return a new query based on this query that skips the first n records.
     * @param skipCount The number of records to skip.
     * @return A new EntityQuery
     */
    public EntityQuery Skip(int skipCount) {
      var eq = new EntityQuery(this);
      eq._skipCount = skipCount;
      return eq;
    }

    /**
     * Return a new query based on this query that either adds or removes the inline count capability. 
     * @param inlineCountEnabled Whether to enable inlineCount.
     * @return A new EntityQuery
     */
    public EntityQuery EnableInlineCount(bool inlineCountEnabled) {
      var eq = new EntityQuery(this);
      eq._inlineCountEnabled = inlineCountEnabled;
      return eq;
    }

    /**
     * Return a new query based on this query with the specified resourceName 
     * @param resourceName The name of the url resource.
     * @return A new EntityQuery
     */
    public EntityQuery WithResourceName(string resourceName) {
      var eq = new EntityQuery(this);
      eq._resourceName = resourceName;
      return eq;
    }


    
    private List<string> ToStringList(object src) {
      if (src == null)
        return null;
      if (src is List<string>) {
        return (List<string>)src;
      } else if (src is string) {
        var list = new List<string>();
        list.Add(src as string);
        return list;
         
      }
      throw new Exception("Unable to convert to a List<String>");
    }

    /**
     * Validates that all of the clauses that make up this query are consistent with the 
     * specified EntityType.
     * @param entityType A EntityType
     */
    public void Validate(Type entityType) {
      _entityType = entityType;
      if (_wherePredicate != null) {
        _wherePredicate.Validate(entityType);
      }
      if (_orderByClause != null) {
        _orderByClause.Validate(entityType);
      }
      if (_selectClause != null) {
        _selectClause.Validate(entityType);
      }
    }


    /**
     * Returns the EntityType that this query has been validated against. Not that this property
     * will return null until the validate method has been called.
     * @return The EntityType that this query has been validated against.
     */
    public Type EntityType {
      get { return _entityType; }
    }



    public string ResourceName {
      get { return _resourceName; }
    }

    public BasePredicate WherePredicate {
      get { return _wherePredicate; }
    }

    public OrderByClause OrderByClause {
      get { return _orderByClause; }
    }

    public ExpandClause ExpandClause {
      get { return _expandClause; }
    }

    public SelectClause SelectClause {
      get { return _selectClause; }
    }

    public int? SkipCount {
      get { return _skipCount; }
    }

    public int? TakeCount {
      get { return _takeCount; }
    }

    public bool IsInlineCountEnabled {
      get { return _inlineCountEnabled.HasValue && _inlineCountEnabled.Value; }
    }

    public IDictionary<string, object> GetParameters() {
      return _parameters;
    }

  }
}



